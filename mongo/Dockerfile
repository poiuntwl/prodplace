# Use the official MongoDB image as the base image
FROM mongo:latest

# Copy the initialization script
COPY ./init.js /docker-entrypoint-initdb.d/

# Expose the default MongoDB port
EXPOSE 27017

# Set the default command to run MongoDB
CMD ["mongod"]
