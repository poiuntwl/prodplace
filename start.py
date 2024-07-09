import subprocess
import sys

def run_command(command):
    try:
        subprocess.run(command, check=True, shell=True)
    except subprocess.CalledProcessError as e:
        print(f"Error executing command: {command}")
        print(f"Error details: {e}")
        sys.exit(1)

def main():
    print("Starting Docker Compose process...")

    print("Step 1: Stopping and removing containers, networks, volumes, and images...")
    run_command("docker-compose down --volumes")

    print("Step 2: Building images without using cache...")
    run_command("docker-compose build --no-cache")

    print("Step 3: Creating and starting containers...")
    run_command("docker-compose up")

if __name__ == "__main__":
    main()
