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

If you don't have a PC or other gear sitting around, you may use your phone instead. Using [Termux](https://termux.dev/en/) and a program called [proot-distro](https://github.com/termux/proot-distro), you create a Debian virtual machine and configure .NET to run the bot.

> [!NOTE] > **Time required:** 30-45 minutes  
> **Difficulty:** Intermediate  
> **Prerequisites:** Basic command line knowledge

> [!TIP] > **Performance:** This solution works well for small to medium bots. Resource-intensive bots may drain your battery or experience performance issues. For high-traffic production bots, consider traditional hosting.

For anyone interested, the instructions are detailed below:

#### Requirements

-   A phone with Android 7 or higher (5+ is possible but not recommended as it poses security issues).
-   Termux installed from [F-Droid](https://f-droid.org/en/packages/com.termux/) (NOT from Google Play Store - the Play Store version is outdated).
-   At least 2GB of free storage space.
-   An internet connection.
-   Basic understanding of bash and nano.

> [!WARNING] > **Security:** Your bot token will be stored on your phone. Ensure your device is secured with a lock screen password.

#### Setup

-   Initialize Termux:

```sh
pkg update && pkg upgrade -y
```

> [!TIP]
> It might ask you for input, just click enter and let the default option be executed.

-   Install proot-distro package:

```sh
pkg install proot-distro -y
```

Verify installation:

```sh
proot-distro list
```

You should see available distributions including `debian`.

-   Install a Debian Virtual Machine (VM):

```sh
proot-distro install debian
```

> [!NOTE]
> Installation time will depend on your internet speed.

-   Login into Debian:

```sh
proot-distro login debian
```

-   Initialize Debian:

```sh
apt update -y && apt upgrade -y
```

-   Install Git and Wget:

```sh
apt install git wget -y
```

-   Install .NET via a script for simplicity:

```sh
wget https://dot.net/v1/dotnet-install.sh -O dotnet.sh && chmod +x ./dotnet.sh
```

-   Install .NET 9:

```sh
./dotnet.sh --channel 9.0
```

> [!CAUTION]
> The following steps are essential to get .NET working.

-   Add .NET to path and mitigate GC heap error. Open your `.bashrc` file:

```sh
nano ~/.bashrc
```

> [!TIP] > **Using nano:** Use arrow keys to navigate. To save and exit: press `Ctrl+X`, then `Y` to confirm, then `Enter`.

Add these lines **at the end of the file**:

```sh
export PATH=$PATH:$HOME/.dotnet
export DOTNET_GCHeapHardLimit=1C0000000
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
```

Save and exit the file.

-   Apply the changes:

```sh
source ~/.bashrc
```

-   Verify .NET is working:

```sh
dotnet --version
```

You should see version 9.x.x displayed.

##### Cloud Hosted Code

-   If you have your code hosted on platforms that use Git or are able to download it, get the source code and `cd` into the directory:

```sh
git clone https://github.com/Aiko-IT-Systems/DisCatSharp.Examples/
cd DisCatSharp.Examples
```

-   Add your configuration file, then build the project. To keep the bot online 24/7, check [here](#building-running).

##### Local Source Code

-   If your source code is not hosted on the cloud, you can move the source code inside the VM by following the given steps:

-   Enable developer options on your phone ([find out how](https://developer.android.com/studio/debug/dev-options)), then enable USB debugging. You can find it by scrolling in "Developer Options".

-   Connect your phone to your computer using USB. Open a file manager and drag your project files into your phone's home directory.

-   To move the project files from your phone's storage into Termux, you'll need to set up storage permissions. Follow this [video guide](https://youtu.be/MMeM7szKt44), or use these steps:

    1. In Termux, run: `termux-setup-storage`
    2. Grant storage permissions when prompted
    3. Verify access: `ls ~/storage/shared/`
    4. Your phone's files should now be accessible

-   Once you can see your files in `~/storage/shared/`, copy your project folder there using your phone's file manager.

-   Now, exit out of Debian. Inside Termux, create an environment variable that points to the VM's root directory. This allows you to access the VM's files from Termux. Add the following to your `.bashrc` file and update your session:

```sh
nano ~/.bashrc
export PROOTDISTROFS=/data/data/com.termux/files/usr/var/lib/proot-distro/installed-rootfs
source ~/.bashrc
```

-   Now, move the source code from your phone to the VM. The files should be in `~/storage/shared/`. Replace `projectname` with your actual project folder name:

```sh
mv ~/storage/shared/projectname/ $PROOTDISTROFS/debian/root/
```

-   Log back into Debian and verify the files are there:

```sh
proot-distro login debian
ls
```

-   Add your configuration file, then build the project.

#### Building and Running {#building-running}

> [!WARNING]
> If you have folders such as `bin/` and `obj/` from your prior builds, delete those by running `rm -rf bin/ obj/`. You might run into issues otherwise.

-   Build your project:

```sh
dotnet build
```

-   Run your bot:

```sh
dotnet run
```

**Keeping the bot running 24/7:**

If you close Termux, your bot will stop. To keep it running in the background, install and use `screen`:

```sh
apt install screen -y
screen -S mybot
dotnet run
```

Press `Ctrl+A` then `D` to detach from the screen session. Your bot will continue running in the background.

To reattach later and check on your bot:

```sh
screen -r mybot
```

#### Troubleshooting

**Problem: `dotnet: command not found`**

Solution: The .NET path wasn't added correctly. Verify the exports are in your `.bashrc`:

```sh
cat ~/.bashrc | grep dotnet
```

If nothing appears, re-add the export and source your `.bashrc` again.

---

**Problem: Error 0x8007000E or ICU package errors**

Solution: The globalization environment variables weren't set. Check your `.bashrc`:

```sh
cat ~/.bashrc | grep DOTNET
```

Ensure these lines exist:

```sh
export DOTNET_GCHeapHardLimit=1C0000000
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
```

If missing, add them, save, and run `source ~/.bashrc`.

---

**Problem: Bot stops when I close Termux**

Solution: You need to use `screen` to keep the bot running in the background. See the [keeping bot running section](#building-running).

---

**Problem: Cannot find project files after moving them**

Solution: Verify the files are in the correct location:

```sh
proot-distro login debian
ls ~/
```

If not there, check your source location:

```sh
exit  # Exit Debian back to Termux
ls ~/storage/shared/
```

---

**Problem: `mv: cannot stat` error when moving files**

Solution: The path or filename is incorrect. List your storage contents:

```sh
ls ~/storage/shared/
```

Use the exact folder name shown in your `mv` command.

---

**Problem: Termux crashes or closes unexpectedly**

Solution: This is often due to Android's battery optimization. Go to your phone's Settings > Apps > Termux > Battery and disable battery optimization for Termux.

---

**Problem: `screen: command not found`**

Solution: You're trying to use screen without installing it. Make sure you're logged into Debian and run:

```sh
apt install screen -y
```

#### Support

If you followed all steps correctly, your bot should now be running. For support or any inquiries, you can join the [Discord](https://discord.gg/RXA6u3jxdU).

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
