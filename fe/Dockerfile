FROM oven/bun:latest AS build

WORKDIR /app

# Copy package.json and bun.lock (if exists)
COPY package.json ./
COPY bun.lock ./

RUN bun install

# Copy the rest of the application
COPY . .

# Build the application
RUN bun run build

# Use a lightweight server to serve the dist directory
RUN bun add -g serve

# Expose the port
EXPOSE 12345

# Serve the production build
CMD ["serve", "-s", "dist", "-l", "12345"]
