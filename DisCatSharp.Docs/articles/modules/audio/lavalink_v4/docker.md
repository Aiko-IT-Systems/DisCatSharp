---
uid: modules_audio_lavalink_v4_docker
title: Lavalink V4 Docker
author: DisCatSharp Team
---

# Template for running Lavalink in Docker

- Create a folder where you'd save the lavalink config and plugins. I.e. `mkdir /opt/lavalink`.
- Follow the [Setup](xref:modules_audio_lavalink_v4_setup) instructions to create your `application.yml`. Make sure to set `0.0.0.0` as `address`.
- Save the `application.yml` in the created folder.
- Create a `docker-compose.yml` file like this and save it in the created folder:
```yml
services:
  lavalink:
    image: ghcr.io/lavalink-devs/lavalink:latest
    container_name: lavalink
    restart: unless-stopped
    environment:
      - _JAVA_OPTIONS=-Xmx6G
      - SERVER_PORT=2333
    volumes:
      - ./application.yml:/opt/Lavalink/application.yml
      - ./plugins/:/opt/Lavalink/plugins/
    networks:
      - lavalink
    expose:
      - 2333
    ports:
      - "2333:2333"
    extra_hosts:
      - "host.docker.internal:host-gateway"
networks:
  lavalink:
    name: lavalink
```
- Create a folder called `plugins` in the created folder.
- Run `docker compose up -d` to start lavalink
