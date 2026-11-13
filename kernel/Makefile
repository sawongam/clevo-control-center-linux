# SPDX-License-Identifier: GPL-2.0-only
# Copyright (C) 2026 ripple-cc contributors

obj-m := ripple_dchu.o
KDIR ?= /lib/modules/$(shell uname -r)/build
all:
	$(MAKE) -C $(KDIR) M=$(CURDIR) modules
clean:
	$(MAKE) -C $(KDIR) M=$(CURDIR) clean
