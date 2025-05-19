FROM nginx:alpine

# Copy the static website files to the nginx html directory
COPY . /usr/share/nginx/html/

# Copy our custom nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"] 