FROM node:16-slim

WORKDIR /app

RUN npm install express bcryptjs dotenv

COPY ./public /app/public
COPY ./config.json /app/public/config/config.json

EXPOSE 8083

CMD ["node", "public/server.js"]
