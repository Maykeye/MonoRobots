#!/usr/bin/env bash
if [ z"$1" != z"" ]; then
    dotnet test --filter "FullyQualifiedName~$1"
    exit $?
fi

dotnet test
