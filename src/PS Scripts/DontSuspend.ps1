powercfg /a



powercfg /hibernate off
powercfg -change -standby-timeout-ac 0
powercfg -change -monitor-timeout-ac 0
powercfg -change -disk-timeout-ac 0





powercfg -setactive SCHEME_MIN

