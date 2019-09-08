# BNS
Brandon's Name Server

Simple domain name server and client. 

---

## How to build

```bash
make [bns] # build the server
make bnsclient # build the client
```

## How to run

No special usage requirements.

```bash
./bns
```

To query it, use dig:

```bash
dig -p 50037 @127.0.0.1 +noedns +norecurse -t A www.linux.org
```