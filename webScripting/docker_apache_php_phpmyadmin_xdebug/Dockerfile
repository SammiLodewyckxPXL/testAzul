FROM php:8.3-apache

COPY --from=composer /usr/bin/composer /usr/bin/composer

RUN apt-get update && apt-get install -y \
    libzip-dev \
    libicu-dev \
    && docker-php-ext-install pdo pdo_mysql \
    && pecl install xdebug-3.4.2 \
    && docker-php-ext-enable xdebug \
    && a2enmod rewrite \
    && echo "xdebug.mode=debug" >> /usr/local/etc/php/php.ini \
    && echo "xdebug.client_host=host.docker.internal" >> /usr/local/etc/php/php.ini \
    && echo "ServerName localhost" >> /etc/apache2/apache2.conf \
    && service apache2 restart

WORKDIR /var/www/html
