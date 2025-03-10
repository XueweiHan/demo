#!/bin/bash

LOG_FILE=/tmp/app.log
LOG_PIPE=/tmp/logpipe

rm -f $LOG_PIPE
mkfifo $LOG_PIPE    # Create a named pipe for logging

# Start log file rotation in the background
(
    while true; do
        cat $LOG_PIPE >> $LOG_FILE &        # Start a cat process to write the log file
        cat_pid=$!                          # Store the PID of the cat process

        today=$(date +%Y-%m-%d_%H-%M)       # Get the current date in YYYY-MM-DD_HH-MM format
        while [ "$today" = "$(date +%Y-%m-%d_%H-%M)" ]; do
            sleep 1                         # Sleep until the date changes
        done

        kill $cat_pid                       # Kill the cat process, pausing the log file write

        cp $LOG_FILE $LOG_FILE.$today
        : > $LOG_FILE

        # mv $LOG_FILE $LOG_FILE.$today       # Move the current log file to a rotated log with the current date
        gzip $LOG_FILE.$today &              # Compress the rotated log file
    done
) &

# Redirect stdout and stderr to the log pipe
exec > >(tee -a $LOG_PIPE) 2>&1




# main logic, generate logs
i=0
while true; do
    echo "$i --- $(date +'%Y-%m-%d %H:%M:%S') --- VN2"
    i=$((i+1))
    sleep 5
done



# testing
# start script in container
#   docker run -t --rm -v "c:/repos/demo/demo7/log:/src" --name test mcr.microsoft.com/azure-functions/dotnet:4 /src/test.sh

# start tail in other terminal
#   docker exec -t test tail -F /tmp/app.log
