---
uid: misc_hosting
title: Hosting Solutions
author: DisCatSharp Team
---

# 24/7 Hosting Solutions

## Free hosting

If you're looking for free hosts, you've likely considered using [Glitch](https://glitch.com/).
We advise against using these platforms as they are designed to host web services, not Discord bots, and instances from either of these companies will shut down if there isn't enough internet traffic.
Save yourself the headache and don't bother.

Outside of persuading somebody to host your bot, you won't find any good free hosting solutions.

## Self Hosting

If you have access to an unused machine, have the technical know-how, and you also have a solid internet connection, you might consider hosting your bot on your own.
Even if you don't have a space PC on hand, parts to build one are fairly cheap in most regions. You could think of it as a one time investment with no monthly server fees.
Any modern hardware will work just fine, new or used.

Depending on how complex your bot is, you may even consider purchasing a Raspberry Pi ($35).

### Termux

If you don't have a PC or other gear sitting around, you may use your phone instead. Using [Termux](https://termux.dev/en/) and a program called [proot-distro](https://github.com/termux/proot-distro), we create a Debian virtual machine and configure DotNET to run the bot. For anyone interested, the instructions are detailed below:

#### Requirements

- A phone with Android 7 or higher (5+ is possible but, not recommended as it posses security issues).
- Termux.
- An internet connection.

#### Setup

- Initialize Termux.
```sh
pkg update && pkg upgrade -y
```
>[!TIP]
> It might ask you for input, just click enter and let the default option be executed.

- Install proot-distro package.
```sh
pkg install proot-distro -y
```

- Install a Debian Virtual Machine (VM).
```sh
proot-distro install debian
```
>[!NOTE]
> Installation time for anything will depend on your internet speed.

- Login into Debian and initialize it.
```sh
proot-distro login debian
```
```sh
apt update -y && apt upgrade -y
```

- Install git and wget.
```sh
apt install git wget -y
```

- We get the DotNET script.
```sh
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && chmod +x ./dotnet-install.sh
```

- Now, we install the DotNET sdk and runtime.
```sh
./dotnet-install.sh --channel 8.0
```
>[!NOTE]
> If the script doesn't automatically add `dotnet` to the path environment variable, run `export PATH=$PATH:$HOME/.dotnet` and add this command to the end of your `.bashrc` file which can be found in your user directory. To edit the `.bashrc` file run, `nano ~/.bashrc` and paste in the arguments. The `.bashrc` file is your configuration file for the bash shell environment.

>[!WARNING]
> You might encounter the error code `0x8007000E` when you try to run the bot. To fix it, run `export DOTNET_GCHeapHardLimit=1C0000000` and also put that command into your `~/.bashrc` file. 
> You'll possibly also get hit with an error message about a "valid ICU package" to fix that, run `export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1` and run the bot again. Or simply add these lines to your `.csproj` file in your project folder:
```xml
<RuntimeHostConfigurationOption Include="System.Globalization.Invariant" Value="true" />
```

##### Cloud Hosted Code

- If you have your code hosted with git or simply are able to download it, get the source code and `cd` into the directory. Example of a git repository on GitHub:
```sh
git clone https://github.com/username/repo.git
cd repo
```
- Add your configuration file then build the project.

##### Local Source Code

- If you do not have your source code hosted remotely, you can move the source code inside the VM by following the given steps: 

- Enable developer options on your phone, find out [here](https://developer.android.com/studio/debug/dev-options) and also enable USB debugging which you can do by going inside `Settings > Developer options > USB debugging` or where ever your phone's is located.

- Find the option `Default USB configuration` and choose `Transferring files`. Disconnect your phone from the usb and reconnect it again.

- Double click on `Phone` or what it is named, then simply copy your project folder from your pc and paste it in there. If you want to keep it in a custom path, remember the path to the project.

>[!NOTE]
> Transferring of files might take a bit. Be sure to not interrupt the process by shutting down or disconnecting your phone

- First, we setup Termux storage run, `termux-setup-storage` and then it'll prompt you to give Termux access to your files and folders. Then run, `ls ~/storage/shared` to make sure you have access.

>[!WARNING]
> If you do not give Termux access to your files and folders, nothing will work going from here on.

- Now, exit out of Debian and into normal Termux and make a new environment variable which leads us to the VM's root directory. Run, `export PROOTDISTROFS=/data/data/com.termux/files/usr/var/lib/proot-distro/installed-rootfs` and run `source $HOME/.bashrc`.

>[!TIP]
> If you don't have a `.bashrc` file in your $HOME, just run `nano $HOME/.bashrc`, save, and exit. Then run the source command again.

- Now, we'll move over the source code from our phone to the VM. Simply, run, `mv projectname/ $PROOTDISTROFS/debian/root/`.

>[!WARNING]
> Double-check file paths, as you might accidentally move the wrong files around.

- Now, let's log back into Debian `proot-distro login debian` and check if the files are here. To check, you can run `ls` to list the current content of the folder.

- Add your configuration file then build the project.

>[!CAUTION]
> If you have folders such as `bin/` and `obj/` from your prior builds on your PC, delete those by running `rm -rf bin/ obj/`. You might run into issues otherwise.

#### Profit

Bot should be working fine, given you follow appropriate steps. For support or any inquires you can join our discord.

## Third-Party Hosting

The simplest, and probably most hassle-free (and maybe cheapest in the long run for dedicated machines) option is to find a provider
that will lend you their machine or a virtual host so you can run your bot in there.

Generally, cheapest hosting options are all GNU/Linux-based, so it's highly recommended you familiarize yourself with the OS and its
environment, particularly the shell (command line), and concepts such as SSH.

There are several well-known, trusted, and cheap providers:

-   [Host Pls](https://host-pls.com/) - A hosting solution made by Discord bot developers. Based in America, starting from $2.49/mo.
-   [Vultr](https://www.vultr.com/products/cloud-compute/) - Based in the US with datacenters in many regions, including APAC. Starting at $2.50/mo.
-   [DigitalOcean](https://www.digitalocean.com/products/droplets/) - The gold standard, US based. Locations available world wide. Starting from $5.00/mo.
-   [Linode](https://www.linode.com/products/shared/) - US based host with many datacenters around the world. Starting at $5.00/mo.
-   [OVH](https://www.ovhcloud.com/en/vps/) - Very popular VPS host. Based in Canadian with French locations available. Starting from $6.00/mo.
-   [Contabo](https://contabo.com/?show=vps) - Based in Germany; extremely good value for the price. Starting from 4.99€/mo.

Things to keep in mind when looking for a VPS host:

-   The majority of cheap VPS hosts will be running some variant of Linux, and not Windows.
-   The primary Discord API server is located in East US.
    -   If latency matters for you application, choose a host that is closer to this location.

In addition to these, there are several hosting providers that offer free trials or in-service credit:

-   [**Microsoft Azure**](https://azure.microsoft.com/en-us/free/?cdn=disable "Microsoft Azure"): $200 in-service credit,
    to be used within month of registration. Requires credit or debit card for validation. Azure isn't cheap, but it supports
    both Windows and GNU/Linux-based servers. If you're enrolled in Microsoft Imagine, it's possible to get these cheaper or
    free.
-   [**Amazon Web Services**](https://aws.amazon.com/free/ "AWS"): Free for 12 months (with 750 compute hours per month). Not
    cheap once the trial runs out, but it's also considered industry standard in cloud services.
-   [**Google Cloud Platform**](https://cloud.google.com/free/ "Google Cloud Platform"): $300 in-service credit, to be used
    within year of registration. GCP is based in the US, and offers very scalable products. Like the above, it's not the
    cheapest of offerings.

## Hosting on Cloud Native Services

With most bots, unless if you host many of them, they dont require a whole machine to run them, just a slice of a machine. This is
where Docker and other cloud native hosting comes into play. There are many different options available to you and you will need
to chose which one will suit you best. Here are a few services that offer Docker or other cloud native solutions that are cheaper than running
a whole VM.

-   [**Azure App Service**](https://azure.microsoft.com/en-us/services/app-service/ "Azure App Service"): Allows for Hosting Website, Continuous Jobs,
    and Docker images on a Windows base or Linux base machine.
-   [**AWS Fargate**](https://aws.amazon.com/fargate/ "AWS Fargate"): Allows for hosting Docker images within Amazon Web Services
-   [**Jelastic**](https://jelastic.com/docker/ "Jelastic"): Allows for hosting Docker images.

# Making your publishing life easier

Now that we have covered where you can possibly host your application, now lets cover how to make your life easier publishing it. Many different
source control solutions out there are free and also offer some type of CI/CD integration (paid and free). Below are some of the
solutions that we recommend:

-   [**Azure Devops**](https://azure.microsoft.com/en-us/services/devops/?nav=min "Azure Devops"): Allows for GIT source control hosting along with integrated CI/CD
    pipelines to auto compile and publish your applications. You can also use their CI/CD service if your code is hosted in a different source control environment like Github.
-   [**Github**](https://github.com/ "GitHub") Allows for GIT source control hosting. From here you can leverage many different CI/CD options to compile and publish your
    applications.
-   [**Bitbucket**](https://bitbucket.org/ "Bitbucket"): Allows for GIT source control hosting along with integrated CI/CD pipelines to auto compile and publish your applications.
