#!/usr/bin/env bash

git checkout -- . ; git pull ; source .env && sudo docker build -t postmanagerapp . && sudo docker-compose down && sudo docker-compose up -d
