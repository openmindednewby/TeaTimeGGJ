# Stage 1 BUILD
# Stage 2 copy build into nginx


FROM nginx:alpine

# move to the location inside the image where the nginx configuration is
WORKDIR /etc/nginx/conf.d
COPY nginx.conf default.conf

# the folder inside the image that we specifiy in nginx.config
WORKDIR /webgl

# the folder with the build results
COPY buildForWeb/ .

# Stage 3 docker build -t tea-time-ggj -f nginx.prod.dockerfile .
# Stage 4 docker run -dp 4297:80 --name teaTimeGGJ --network=internal-docker --ip=172.18.0.5 tea-time-ggj


