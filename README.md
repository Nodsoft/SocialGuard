<img align="right" src="SocialGuard.Web/wwwroot/assets/icons/android-chrome-512x512.png" alt="logo" width="256"/>

# NSYS SocialGuard

### A centralized Trustlist to keep your server safe from known intruders.  <br />[socialguard.net](https://socialguard.net)


<hr />

NSYS SocialGuard is a centralized Discord Trustlist
designed to keep your server safe from known intruders 
(such as Raiders, Trolls, Chasers, Scammers, Bots, etc).

At its core, SocialGuard is an API that allows Discord server admins and operators 
to rapidly and effectively communicate moderation notes, and bans, with each other.
With bi-directional communication, SocialGuard can also notify other apps of new records,
thus providing near-instant propagation of moderation actions.

### An Open(-Source) System.
SocialGuard is open to use by any actor, and its records are made available to anyone.
Any server admin/operator can use the records from SocialGuard to secure their own servers,
and any developer can integrate SocialGuard into their own apps with ease.  
Security over the records on [socialguard.net](https://socialguard.net) 
is maintained by the Directory committee at NSYS, who manages emitter permissions 
to verified and trusted parties.


## Projects

### SocialGuard API (`SocialGuard.Api`) - [api.socialguard.net](https://api.socialguard.net)

**The central piece of the SocialGuard system.**  
SocialGuard API is a RESTful API enabled with SignalR for real-time communication
with consumer systems. It uses JWT to authenticate requests, and provides a simple
structure for storing and retrieving records. Storage is provided by a PostgreSQL database,
powered by Entity Framework Core.

### SocialGuard Web (`SocialGuard.Web`) - [socialguard.net](https://socialguard.net)
**The front website for NSYS SocialGuard.**  
A simple set of landing pages designed to provide a quick overview of the system, 
and a link to the different resources and integrated apps. 
Currently powered by Blazor, our plans are to move to a more lightweight framework in the future.

### SocialGuard Records Browser (`SocialGuard.Web.Viewer`) - [browser.socialguard.net](https://browser.socialguard.net)
**Web-based browser app for interacting with SocialGuard records.**  
A Blazor WASM-powered browser app for viewing SocialGuard records, 
straight from the API. Our plans moving forward are to flesh out the records browser 
with editing capabilities, along with a richer set of features.

### SocialGuard Common Assets (`SocialGuard.Common`) - [NuGet](https://www.nuget.org/packages/SocialGuard.Common)
**The standards for the SocialGuard system.**
This NuGet package contains the common assets used by the SocialGuard system,
such as API types, DTOs, and other common types. All official projects use this package
as a baseline for implementation, and can be used as a reference for your own .NET projects.
