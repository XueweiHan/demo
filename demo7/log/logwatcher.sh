#!/bin/sh

apps="log-watcher0 log-watcher1 log-watcher2 log-watcher3"

trap 'kill 0' TERM INT

for app in $apps; do
    (
        log=$app.log

        while true; do

            # Wait up to 5 minutes for the pod to become ready
            kubectl wait --for=condition=ready pod -l watch=$app --timeout=5m >/dev/null

            # Save the pod logs to $log in background
            rm -f $log
            kubectl logs --tail=1 -l watch=$app > $log 2>/dev/null &
            kubectlPID=$!

            # Sleep for 5 seconds
            sleep 5

            # Check if the 'kubectl logs' is still running, kill it
            if ps -p $kubectlPID >/dev/null 2>&1; then
                kill $kubectlPID
            fi

            # Check if the log file is empty
            if [ ! -s $log ]; then
                echo "$app: pod logs are stopped"

                # Delete the pod, let it restart
                kubectl delete pod -l watch=$app
            else
                echo "$app: pod logs are running"
            fi

        done
    ) &
done

wait



# app=$1
# ns=$2
# log=$app.log

# while true; do

#     # Wait up to 5 minutes for the pod to become ready
#     kubectl wait --for=condition=ready pod -l app=$app -n $ns --timeout=5m >/dev/null

#     # Save the pod logs to $log in background
#     rm -f $log
#     kubectl logs --tail=1 -l app=$app -n $ns > $log 2>&1 &
#     kubectlPID=$!

#     # Sleep for 5 seconds
#     sleep 5

#     # Check if the 'kubectl logs' is still running, kill it
#     if ps -p $kubectlPID >/dev/null 2>&1; then
#         kill $kubectlPID
#     fi

#     # Check if the log file is empty
#     if [ ! -s $log ]; then
#         echo "$app: pod logs are stopped"

#         # Delete the pod, let it restart
#         kubectl delete pod -l app=$app -n $ns
#     else
#         echo "$app: pod logs are running"
#     fi

# done
