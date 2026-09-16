# CryptoCanut

An amateur command line tool to encrypt and decrypt files

/!\ Use at your own peril. this software is probably full of security vulnerabilities /!\

## File format

 3 bytes : "CCT" magic string
 1 byte  : version
16 bytes : salt
 N bytes : encrypted payload

## Payload format

 1 byte  : version
 2 bytes : file name length
 8 bytes : payload length
11 bytes : "CRYPTOCANUT" magic ASCII string
 N bytes : file name in UTF-8
 N bytes : payload
 N bytes : padding (random bytes)

## Padding

- increase file size to the nearest power of two
- Minimum file size : 2^11 bytes (half the default size of a cluster in NTFS)

## Command line

### Encrypt

`CryptoCanut -e [FilePath]` or `CryptoCanut --encrypt [FilePath]`
Ask for a password (10 chars min)
ask to repeat the password
generate a file with the same name plus the `.cct` extension in the current folder

### Decrypt

`CryptoCanut -d [FilePath]` or `CryptoCanut --decrypt [FilePath]`
ask for the password
try to decrypt the file (check for the "CRYPTOCANUT" magic string and if the file name & payload length are coherent)
if the decryption fails, show an error message (like "incorrect password or not a CryptoCanut File") and exit
if the file is correct, create a file with the decrypted filename in the current folder
