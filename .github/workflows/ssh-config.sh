#!/bin/bash
set -e

mkdir -p ~/.ssh

pushd $_

echo "$key" > id

chmod 600 id

cat >> config << EOF
Host target
    HostName $host
    Port $port
    User $user
    IdentityFile ~/.ssh/id
EOF

ssh-keyscan -H -p $port $host >> known_hosts
