Generate root CA key

`openssl genrsa -out rootCA.key 4096`

Create CA Request

`openssl req -x509 -new -nodes -key rootCA.key -sha256 -days 1024 -out rootCA.crt`

Generate server key

`openssl genrsa -out localhost.key 2048`

Generate server csr

`openssl req -new -sha256 -key localhost.key -config localhost.conf -out localhost.csr`

Sign and generate cert

`openssl x509 -req -in localhost.csr -CA rootCA.crt -CAkey rootCA.key -CAcreateserial -out localhost.crt -days 500 -sha256 -extfile localhost.conf -extensions req_ext`

Convert to pfx

`openssl pkcs12 -export -out localhost.pfx -inkey localhost.key -in localhost.crt`

Verify

`openssl verify -CAfile localhost.crt localhost.crt`

Copy localhost.crt to /usr/local/share/ca-certificates
Make sure /usr/local/share/ca-certificates has r+x permissions
Run `sudo update-ca-certificates`
Verify: `openssl verify localhost.crt`

Appsettings config
"Kestrel": {
    "Certificates": {
      "Default": {
        "Path": "../../certs/localhost.pfx",
        "Password": ""
      }
    }
  }