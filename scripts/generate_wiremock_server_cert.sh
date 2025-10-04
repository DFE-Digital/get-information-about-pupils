#!/bin/bash

# Generate a local CA (certificate authority)

set -e

echo '[req]
distinguished_name = req_distinguished_name
x509_extensions = v3_ca
[req_distinguished_name]
[v3_ca]
basicConstraints = critical, CA:true
keyUsage = keyCertSign, cRLSign' > ca.conf

openssl genrsa -out ca.key 2048

openssl req -x509 -new -nodes -key ca.key -sha256 -days 3650 -out ca.crt -subj "/CN=MyLocalCA" -extensions v3_ca -config ca.conf

openssl genrsa -out wiremock-cert.key 2048

# Generate CSR (Certificate signing request) to sign WireMock self-generated certificate with local CA

openssl req -new -key wiremock-cert.key -out wiremock-cert.csr -subj '/CN=localhost'

echo 'authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth
subjectAltName = @alt_names
1.3.6.1.4.1.311.84.1.1 = DER:01

[alt_names]
DNS.1 = localhost
DNS.2 = 127.0.0.1' > wiremock-cert.ext

openssl x509 -req -in wiremock-cert.csr -CA ca.crt -CAkey ca.key -CAcreateserial -out wiremock-cert.crt -days 365 -sha256 -extfile wiremock-cert.ext

openssl pkcs12 -export -out wiremock-cert.pfx -inkey wiremock-cert.key -in wiremock-cert.crt -certfile ca.crt -passout pass:yourpassword

# Move certificate into directory where integration tests use

echo 'Printing current directory...'
ls -la .

echo 'Moving signed certificated localhost.pfx into Integration tests output directory...'
mv wiremock-cert.pfx DfE.GIAP.All/tests/DfE.GIAP.Core.IntegrationTests/bin/Debug/net8.0
echo 'Moved signed certificate'

echo 'Copying local CA into the host trusted CA store'
mv ca.crt /usr/local/share/ca-certificates/my-local-ca.crt
echo 'Copied local CA'
