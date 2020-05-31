#!/usr/bin/env bash

git checkout -- . ; source .env && git pull && sudo docker build -t postmanagerapp . && sudo docker-compose down && sudo docker-compose up -d
