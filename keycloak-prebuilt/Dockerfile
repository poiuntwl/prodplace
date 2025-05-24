FROM quay.io/keycloak/keycloak:26.2.4 AS builder

# Configure for testing environment
ENV KC_HEALTH_ENABLED=true
ENV KC_METRICS_ENABLED=false

# Build optimized image
RUN /opt/keycloak/bin/kc.sh build

FROM quay.io/keycloak/keycloak:26.2.4
COPY --from=builder /opt/keycloak/ /opt/keycloak/

ENV KC_HTTP_ENABLED=true
ENV KC_HOSTNAME_STRICT_HTTPS=false
ENV KC_DB=postgres
ENV KC_DB_URL=jdbc:postgresql://keycloak-db:5432/keycloak_db

# Use optimized startup
ENTRYPOINT ["/opt/keycloak/bin/kc.sh", "start"]