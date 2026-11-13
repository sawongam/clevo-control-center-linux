// SPDX-License-Identifier: GPL-2.0
/*
 * Copyright (C) 2026 ripple-cc contributors
 */
/*
 * ripple_dchu - expose the Clevo "DCHU" ACPI _DSM interface to userspace.
 *
 * Windows Control Center 3.0 talks to the firmware through AcpiBridge.sys,
 * which evaluates \_SB.DCHU._DSM (HID CLV0001) with
 *   UUID 93f224e4-fbdc-4bbf-add6-db71bdc0afad, function = DCHU command,
 *   Arg3 = Package { Integer arg }        (simple commands, e.g. 0x79)
 *   Arg3 = Package { Buffer[256] }        (package commands, e.g. 0x0E fan table)
 *
 * The argument encoding mirrors tuxedo-drivers' clevo_acpi.c, which is known
 * to work on this firmware. Root-only misc device /dev/ripple_dchu.
 */
#include <linux/module.h>
#include <linux/acpi.h>
#include <linux/miscdevice.h>
#include <linux/fs.h>
#include <linux/mutex.h>
#include <linux/uaccess.h>
#include <linux/slab.h>
#include <linux/uuid.h>

#include "ripple_dchu.h"

#define DCHU_PATH "\\_SB.DCHU"
#define DCHU_UUID "93f224e4-fbdc-4bbf-add6-db71bdc0afad"

static acpi_handle dchu_handle;
static guid_t dchu_guid;
static DEFINE_MUTEX(dchu_lock);

static int dchu_call(struct ripple_dchu_call *c)
{
	union acpi_object elem, pkg, *out;

	if (c->in_type == RIPPLE_DCHU_IN_INT) {
		elem.integer.type = ACPI_TYPE_INTEGER;
		elem.integer.value = c->in_int;
	} else if (c->in_type == RIPPLE_DCHU_IN_BUF) {
		if (c->in_len == 0 || c->in_len > RIPPLE_DCHU_BUF_MAX)
			return -EINVAL;
		elem.buffer.type = ACPI_TYPE_BUFFER;
		elem.buffer.length = c->in_len;
		elem.buffer.pointer = c->in_buf;
	} else {
		return -EINVAL;
	}

	pkg.package.type = ACPI_TYPE_PACKAGE;
	pkg.package.count = 1;
	pkg.package.elements = &elem;

	out = acpi_evaluate_dsm(dchu_handle, &dchu_guid, 0, c->func, &pkg);
	if (!out)
		return -EIO;

	c->out_type = RIPPLE_DCHU_OUT_NONE;
	c->out_int = 0;
	c->out_len = 0;

	switch (out->type) {
	case ACPI_TYPE_INTEGER:
		c->out_type = RIPPLE_DCHU_OUT_INT;
		c->out_int = out->integer.value;
		break;
	case ACPI_TYPE_BUFFER:
		c->out_type = RIPPLE_DCHU_OUT_BUF;
		c->out_len = min_t(u32, out->buffer.length, RIPPLE_DCHU_BUF_MAX);
		memcpy(c->out_buf, out->buffer.pointer, c->out_len);
		break;
	default:
		break;
	}

	ACPI_FREE(out);
	return 0;
}

static long dchu_ioctl(struct file *f, unsigned int cmd, unsigned long arg)
{
	struct ripple_dchu_call *c;
	int ret;

	if (cmd != RIPPLE_DCHU_IOC_CALL)
		return -ENOTTY;
	if (!capable(CAP_SYS_ADMIN))
		return -EPERM;

	c = kzalloc(sizeof(*c), GFP_KERNEL);
	if (!c)
		return -ENOMEM;

	if (copy_from_user(c, (void __user *)arg, sizeof(*c))) {
		ret = -EFAULT;
		goto out;
	}

	mutex_lock(&dchu_lock);
	ret = dchu_call(c);
	mutex_unlock(&dchu_lock);

	if (!ret && copy_to_user((void __user *)arg, c, sizeof(*c)))
		ret = -EFAULT;
out:
	kfree(c);
	return ret;
}

static const struct file_operations dchu_fops = {
	.owner = THIS_MODULE,
	.unlocked_ioctl = dchu_ioctl,
	.compat_ioctl = compat_ptr_ioctl,
};

static struct miscdevice dchu_dev = {
	.minor = MISC_DYNAMIC_MINOR,
	.name = "ripple_dchu",
	.fops = &dchu_fops,
	.mode = 0600,
};

static int __init ripple_dchu_init(void)
{
	acpi_status st;

	st = acpi_get_handle(NULL, DCHU_PATH, &dchu_handle);
	if (ACPI_FAILURE(st)) {
		pr_err("ripple_dchu: %s not found\n", DCHU_PATH);
		return -ENODEV;
	}
	if (guid_parse(DCHU_UUID, &dchu_guid))
		return -EINVAL;
	if (!acpi_check_dsm(dchu_handle, &dchu_guid, 0, 0) &&
	    !acpi_has_method(dchu_handle, "_DSM")) {
		pr_err("ripple_dchu: no _DSM on %s\n", DCHU_PATH);
		return -ENODEV;
	}

	pr_info("ripple_dchu: bound to %s\n", DCHU_PATH);
	return misc_register(&dchu_dev);
}

static void __exit ripple_dchu_exit(void)
{
	misc_deregister(&dchu_dev);
}

module_init(ripple_dchu_init);
module_exit(ripple_dchu_exit);

MODULE_DESCRIPTION("Clevo DCHU ACPI _DSM userspace bridge (Ripple Control Center port)");
MODULE_AUTHOR("ripple-cc contributors");
MODULE_LICENSE("GPL");
