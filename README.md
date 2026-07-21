# Industrial MES Workstation System

## Overview

This is a standalone industrial workstation system.
Each OP module runs independently.

## Architecture

- WinForm desktop application
- Shared core library: GT_Common
- Serilog-based logging
- Local database access

## Logging

- Daily rolling file logs
- UI real-time display
- MES channel logs (Send / Receive)
- No centralized logging

## Deployment

Each OP project is deployed separately.

## Version

v2.1 - Stable Release
