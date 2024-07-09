# Use the official MongoDB image as the base image
FROM mongo:latest

# Set environment variables
ENV MONGO_INITDB_DATABASE=mydb

# Copy the initialization script
COPY ./init.js /docker-entrypoint-initdb.d/

# Expose the default MongoDB port
EXPOSE 27017

# Set the default command to run MongoDB
CMD ["mongod"]
