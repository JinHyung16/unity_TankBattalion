FROM heroiclabs/nakama-pluginbuilder:3.14.0 AS go-builder

ENV GO111MODULE on
ENV CGO_ENABLED 1

WORKDIR /HughServer

COPY go.mod .
COPY go.sum .
COPY *.go .
COPY vendor/ vendor/

RUN go mod download
RUN go build --trimpath --mod=vendor --buildmode=plugin -o ./HughServer.so

FROM heroiclabs/nakama:3.14.0

COPY --from=go-builder /HughServer/HughServer.so /nakama/data/modules/build/
COPY local.yml /nakama/data/