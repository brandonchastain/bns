CC=gcc
CFLAGS=-g -I.
DEPS = util.h common.h
OBJ = bns.o util.o
CLIENTOBJ = bnsclient.o util.o

%.o: %.c $(DEPS)
	$(CC) -c -o $@ $< $(CFLAGS)

bns: $(OBJ)
	$(CC) -o $@ $^ $(CFLAGS)

bnsclient: $(CLIENTOBJ)
	$(CC) -o $@ $^ $(CFLAGS)

test: test.o util.o
	$(CC) -o $@ $^ $(CFLAGS)

clean:
	rm -f *.o
	rm -f *.out
	rm -f bnsclient
	rm -f bns
	rm -f test
