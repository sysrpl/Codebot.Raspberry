# Raspsberry Pi Dotnet Core Projects and Tools

This repository contains projects and tools for working with Raspberry Pi GPIO pins and a collection of hardware peripherals.

## Getting your Raspberry Pi Configured

Before using this repository you should configure your Pi to require a ssh key for secure and fast remote connections.

<details>
  <summary>How to configure your Pi to support ssh with key files</summary>
On your Pi should enable ssh, sshfs, and generate a secure key on using these commands:

```bash
# enable and start the ssh server
sudo systemctl enable ssh
sudo systemctl start ssh
# install the tool for sshfs, the file system through ssh
sudo apt install sshfs
# generate ssh keys
mkdir /home/pi/.ssh
cd /home/pi/.ssh
ssh-keygen -t rsa
mv id_rsa.pub authorized_keys
chmod 600 authorized_keys
```
</details>
<details>
  <summary>How to configure your Linux computer to use the Pi key files</summary>
On your Linux machine temporarily connect to your Pi using ssh with password authentication and enabled sshfs for secure file transfers. Where you see ``1.2.3.4`` substitute it with  the ip address of your Pi:

```bash
# go to you home folder and install the tool for sshfs
cd ~
sudo apt install sshfs
# create a mount poitn for your Raspberry Pi home folder
mkdir -p media/raspberry
sshfs pi@1.2.3.4:/home/pi media/raspberry
# setup ssh with the correct key on your Linux machine
mkdir .ssh
openssl rsa -in media/raspberry/.ssh/id_rsa -outform pem > .ssh/pi.pem
chmod 600 .ssh/pi.pem
touch .ssh/config
chmod 600 
```

On your Linux machine use a text editor to add these lines to  ``~/.ssh/config``:

```bash
Host pi
    HostName 1.2.3.4
    User pi
    IdentityFile ~/.ssh/pi.pem
```
</details>
<details>
  <summary>Testing and disabling password authentication</summary>

Now that we have ssh configured you should be able to connect to your Pi using a secure key:

```bash
ssh pi
```

Finally on your Pi edit ``sshd_config`` to remove password authentication:

```bash
sudo nano /etc/ssh/sshd_config.
# edit these values
PasswordAuthentication no
PubkeyAuthentication yes
# save and reboot your Pi
sudo reboot
```

If you reboot your Linux computer you can reconnect to the Pi file system using:

```bash
ssh pi:/home/pi ~/media/raspberry
```
</details>

