/* SPDX-License-Identifier: GPL-2.0 WITH Linux-syscall-note */
/*
 * Copyright (C) 2026 ripple-cc contributors
 */
#ifndef RIPPLE_DCHU_H
#define RIPPLE_DCHU_H

#include <linux/types.h>
#include <linux/ioctl.h>

#define RIPPLE_DCHU_BUF_MAX 256

#define RIPPLE_DCHU_IN_INT 0
#define RIPPLE_DCHU_IN_BUF 1

#define RIPPLE_DCHU_OUT_NONE 0
#define RIPPLE_DCHU_OUT_INT  1
#define RIPPLE_DCHU_OUT_BUF  2

/* Layout is mirrored by ripplectl (Python struct "<IIII256sIIQ256s"). */
struct ripple_dchu_call {
	__u32 func;      /* DCHU command (_DSM function index) */
	__u32 in_type;   /* RIPPLE_DCHU_IN_* */
	__u32 in_int;    /* integer argument */
	__u32 in_len;    /* buffer argument length */
	__u8  in_buf[RIPPLE_DCHU_BUF_MAX];
	__u32 out_type;  /* RIPPLE_DCHU_OUT_* */
	__u32 out_len;
	__u64 out_int;
	__u8  out_buf[RIPPLE_DCHU_BUF_MAX];
};

#define RIPPLE_DCHU_IOC_CALL _IOWR('R', 0x01, struct ripple_dchu_call)

#endif
