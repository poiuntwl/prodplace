FROM oven/bun:latest AS build

WORKDIR /app

COPY package.json ./
COPY bun.lock ./

RUN bun install

COPY . .

RUN bun run build

RUN bun add -g serve

EXPOSE 12346

CMD ["serve", "-s", "dist", "-l", "12346"]
