## HttpMessageSigning [<img src="https://dalion.eu/dalion128.png" align="right" width="48">](https://www.dalion.eu)

A C# implementation of the "Authorization" scheme of the IETF Internet-Draft [Signing HTTP Messages](https://tools.ietf.org/html/draft-ietf-httpbis-message-signatures-00).

[![Nuget](https://img.shields.io/nuget/v/Dalion.HttpMessageSigning)](https://www.nuget.org/packages/Dalion.HttpMessageSigning/) [![Build status](https://ci.appveyor.com/api/projects/status/d8fdl40nfj62ed1v?svg=true)](https://ci.appveyor.com/project/DavidLievrouw/httpmessagesigning) [![Coverage Status](https://coveralls.io/repos/github/DavidLievrouw/HttpMessageSigning/badge.svg?branch=master)](https://coveralls.io/github/DavidLievrouw/HttpMessageSigning?branch=master) [![AppVeyor tests](http://canllp.ca/appveyor/tests/DavidLievrouw/httpmessagesigning)](https://ci.appveyor.com/project/DavidLievrouw/httpmessagesigning) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) 

It contains:
  - HTTP request signing services.
  - HTTP request signature verification services.
  - Authentication middleware for ASP.NET Core applications.
  - Authentication middleware for OWIN applications.
  - Extensions for storing known clients in memory.
  - Extensions for storing known clients in MongoDb.

See [wiki](https://github.com/DavidLievrouw/HttpMessageSigning/wiki) for further details.

## Motivation
When communicating over the Internet using the HTTP protocol, it can be desirable for a server or client to authenticate the sender of a particular message.  It can also be desirable to ensure that the message was not tampered with during transit. The Signing HTTP Messages Internet-Draft describes a way for servers and clients to simultaneously add authentication and message integrity to HTTP messages by using a digital signature.

This repository is a C# implementation of that specification.

## NuGet

#### Common

- [![Nuget](https://img.shields.io/nuget/v/Dalion.HttpMessageSigning?label=Dalion.HttpMessageSigning)](https://www.nuget.org/packages/Dalion.HttpMessageSigning/) [![Nuget](https://img.shields.io/nuget/dt/Dalion.HttpMessageSigning)](https://www.nuget.org/packages/Dalion.HttpMessageSigning/)
<br/><sub>Shared components for signing and verification packages.</sub>

#### Signing

- [![Nuget](https://img.shields.io/nuget/v/Dalion.HttpMessageSigning?label=Dalion.HttpMessageSigning.Signing)](https://www.nuget.org/packages/Dalion.HttpMessageSigning.Signing/) [![Nuget](https://img.shields.io/nuget/dt/Dalion.HttpMessageSigning.Signing)](https://www.nuget.org/packages/Dalion.HttpMessageSigning.Signing/)
<br/><sub>Signing HTTP messages, client-side.</sub>

#### Verification

- [![Nuget](https://img.shields.io/nuget/v/Dalion.HttpMessageSigning?label=Dalion.HttpMessageSigning.Verification)](https://www.nuget.org/packages/Dalion.HttpMessageSigning.Verification/) [![Nuget](https://img.shields.io/nuget/dt/Dalion.HttpMessageSigning.Verification)](https://www.nuget.org/packages/Dalion.HttpMessageSigning.Verification/)
<br/><sub>Verify request signatures, server-side, including support for in-memory client and nonce stores.</sub>

- [![Nuget](https://img.shields.io/nuget/v/Dalion.HttpMessageSigning?label=Dalion.HttpMessageSigning.Verification.AspNetCore)](https://www.nuget.org/packages/Dalion.HttpMessageSigning.Verification.AspNetCore/) [![Nuget](https://img.shields.io/nuget/dt/Dalion.HttpMessageSigning.Verification.AspNetCore)](https://www.nuget.org/packages/Dalion.HttpMessageSigning.Verification.AspNetCore/)
<br/><sub>ASP.NET Core authentication scheme to verify signatures on signed HTTP messages.</sub>

- [![Nuget](https://img.shields.io/nuget/v/Dalion.HttpMessageSigning?label=Dalion.HttpMessageSigning.Verification.Owin)](https://www.nuget.org/packages/Dalion.HttpMessageSigning.Verification.Owin/) [![Nuget](https://img.shields.io/nuget/dt/Dalion.HttpMessageSigning.Verification.Owin)](https://www.nuget.org/packages/Dalion.HttpMessageSigning.Verification.Owin/)
<br/><sub>Owin authentication middleware to verify signatures on signed HTTP messages.</sub>

#### Storage

- [![Nuget](https://img.shields.io/nuget/v/Dalion.HttpMessageSigning?label=Dalion.HttpMessageSigning.Verification.MongoDb)](https://www.nuget.org/packages/Dalion.HttpMessageSigning.Verification.MongoDb/) [![Nuget](https://img.shields.io/nuget/dt/Dalion.HttpMessageSigning.Verification.MongoDb)](https://www.nuget.org/packages/Dalion.HttpMessageSigning.Verification.MongoDb/)
<br/><sub>MongoDB-backed client and nonce store implementations, as an alternative to the default in-memory ones.

## Basics
When signing a request message, an _Authorization_ header is set in a http request. Using this header, the server can verify that it is sent by the known client, and that the content has not been tampered with.

The signing will result in a request header that will look like:

```
Authorization: Signature keyId="e0e8dcd638334c409e1b88daf821d135",algorithm="hs2019",created=1584806516,expires=1584806576,headers="(request-target) dalion-app-id date digest",nonce="38brRy8BLUajMbUqWumXPg",signature="DUKQVjiirGMMaMOy9qIwKMro46R3BlLsvUQkw1/8sKQ="
```

See the [Super Duper Happy Paths](https://github.com/DavidLievrouw/HttpMessageSigning/wiki/Super-duper-happy-paths) for basic usage.

There is OWIN and ASP.NET Core middleware available too, for easy integration.
By default, verification settings are stored in-memory. There are also extension packages to store data in Sql Server, MongoDB, ... instead.

## Documentation

See [Wiki](https://github.com/DavidLievrouw/HttpMessageSigning/wiki).

## Support

If you've got value from any of the content which I have created, but pull requests are not your thing, then I would also very much appreciate your support by buying me a coffee.

<a href="https://www.buymeacoffee.com/DavidLievrouw" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: 41px !important;width: 174px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;" ></a>

---
"Anybody can make something that works. Software craftsmanship is the ability to keep it understandable, maintainable and extensible."
