import subprocess
import sys

def run_command(command):
    try:
        subprocess.run(command, check=True, shell=True)
    except subprocess.CalledProcessError as e:
        print(f"Error executing command: {command}")
        print(f"Error details: {e}")
        sys.exit(1)

def rebuild_and_restart(container_name):
    print(f"Rebuilding and restarting container: {container_name}")

    # Stop the container
    print("Stopping the container...")
    run_command(f"docker-compose stop {container_name}")

    # Remove the container
    print("Removing the container...")
    run_command(f"docker-compose rm -f {container_name}")

    # Rebuild the container
    print("Rebuilding the container...")
    run_command(f"docker-compose build --no-cache {container_name}")

    # Start the container
    print("Starting the container...")
    run_command(f"docker-compose up -d {container_name}")

    print(f"Container {container_name} has been rebuilt and restarted.")

if __name__ == "__main__":
    container_name = 'productsservice'
    rebuild_and_restart(container_name)