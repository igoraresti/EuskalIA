adb install -r EuskalLingo.apk
## Clean log -> adb logcat -c
## Filter logs -> adb logcat "*:E" | grep -iE "euskal|react|fatal|javascript"