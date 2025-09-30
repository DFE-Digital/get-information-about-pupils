#!/bin/bash

# Generate a local CA (certificate authority)

set -e

echo "[req]
distinguished_name = req_distinguished_name
x509_extensions = v3_ca
[req_distinguished_name]
[v3_ca]
basicConstraints = critical, CA:true
keyUsage = keyCertSign, cRLSign" > ca.conf

openssl genrsa -out ca.key 2048

openssl req -x509 -new -nodes -key ca.key -sha256 -days 3650 -out ca.crt -subj "/CN=MyLocalCA" -extensions v3_ca -config ca.conf

openssl genrsa -out localhost.key 2048

# Generate CSR (Certificate signing request) to sign WireMock self-generated certificate with local CA

openssl req -new -key localhost.key -out localhost.csr -subj "/CN=localhost"

echo "authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth
subjectAltName = @alt_names
1.3.6.1.4.1.311.84.1.1 = DER:01

[alt_names]
DNS.1 = localhost
DNS.2 = 127.0.0.1" > localhost.ext

openssl x509 -req -in localhost.csr -CA ca.crt -CAkey ca.key -CAcreateserial -out localhost.crt -days 365 -sha256 -extfile localhost.ext

openssl pkcs12 -export -out localhost.pfx -inkey localhost.key -in localhost.crt -certfile ca.crt -passout pass:yourpassword

# Move certificate into directory where integration tests use

echo "Moving localhost.pfx into Integration tests directory $INTEGRATION_TEST_OUTPUT_DIRECTORY"

ls -la .

mv localhost.pfx $INTEGRATION_TEST_OUTPUT_DIRECTORY

echo 'Copying local CA into host trust store'

sudo cp ca.crt /usr/local/share/ca-certificates/my-local-ca.crt

sudo update-ca-certificates