#!/bin/bash

# Function to check if a service is ready
wait_for_service() {
    SERVICE=$1
    PORT=$2

    echo "Waiting for $SERVICE to be ready..."
    while ! nc -z $SERVICE $PORT; do
        sleep 1
    done
    echo "$SERVICE is ready!"
}

# Check for service readiness
wait_for_service "studybuddy.api" "80"