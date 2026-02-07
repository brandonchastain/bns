# DNS Stub Resolver

Simple implementation of a DNS nameserver in C#. Currently it is only a Stub Resolver, but it is extensible to support other resolution strategies such as recursive queries or responding with authoritatively.


# Running it locally

1. Specify your configuration in `DnsResolver/StubResolver.App/appsettings.json`
2. Run the stub resolver:

```powershell
cd DnsResolver/StubResolver.App/
dotnet run
```

# Testing

* You can run `DnsClient.App` for to do some basic benchmarking against the resolver.
* Also, you can send a diagnostic DNS query to the stub resolver using `dig.exe`:

```powershell
$ dig @192.168.1.1 -p 8080 google.com NS

; <<>> DiG 9.18.30-0ubuntu0.20.04.2-Ubuntu <<>> @192.168.1.1 -p 8080 google.com NS
; (1 server found)
;; global options: +cmd
;; Got answer:
;; ->>HEADER<<- opcode: QUERY, status: NOERROR, id: 31641
;; flags: qr rd ra ad; QUERY: 1, ANSWER: 4, AUTHORITY: 0, ADDITIONAL: 0

;; QUESTION SECTION:
;google.com.                    IN      NS

;; ANSWER SECTION:
google.com.             228     IN      NS      ns1.google.com.
google.com.             228     IN      NS      ns4.google.com.
google.com.             228     IN      NS      ns2.google.com.
google.com.             228     IN      NS      ns3.google.com.

;; Query time: 3 msec
;; SERVER: 192.168.1.1#8080(192.168.1.1) (UDP)
;; WHEN: Sat Feb 07 15:34:09 PST 2026
;; MSG SIZE  rcvd: 180

```