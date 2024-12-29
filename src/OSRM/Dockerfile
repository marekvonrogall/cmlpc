FROM osrm/osrm-backend:latest

WORKDIR /data

# Update APT sources to use archive.debian.org and remove stretch-updates
RUN sed -i 's|http://deb.debian.org/debian|http://archive.debian.org/debian|g' /etc/apt/sources.list && \
    sed -i 's|http://security.debian.org/debian-security|http://archive.debian.org/debian-security|g' /etc/apt/sources.list && \
    sed -i '/stretch-updates/d' /etc/apt/sources.list && \
    apt-get update && apt-get install -y --no-install-recommends wget osmctools && \
    rm -rf /var/lib/apt/lists/*

# DATASET: GERMANY
RUN wget --no-check-certificate -O /data/germany-latest.osm.pbf https://download.geofabrik.de/europe/germany-latest.osm.pbf

# CONVERT
RUN osmconvert /data/germany-latest.osm.pbf -o=/data/germany-latest.osm

# PREPROCESS
RUN osrm-extract -p /opt/car.lua /data/germany-latest.osm && \
    osrm-partition /data/germany-latest.osrm && \
    osrm-customize /data/germany-latest.osrm

EXPOSE 5000

CMD ["osrm-routed", "--algorithm", "mld", "/data/germany-latest.osrm"]
