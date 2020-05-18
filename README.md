# Raspsberry Pi Dotnet Core Projects and Tools

This repository contains projects and tools for working with Raspberry Pi GPIO pins and a collection of hardware peripherals.

## Getting your Raspberry Pi Configured

Before using this repository you should configure your Pi to require a ssh key for secure and fast remote connections.

Enabled ssh, sshfs, and generate a secure key on you Pi using these commands:

```console
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

Your Linux machine can then temporarily connect to you Pi using ssh with password authentication and enabled sshfs for secure file transfers. Where ``1.2.3.4`` is used substitute it the ip address of your Pi:

```console
# go to you home folder and install the tool for sshfs
cd ~
sudo apt install sshfs
# create a mount poitn for your Raspberry Pi home folder
mkdir -p media/raspberry
sshfs pi@1.2.3.4:/home/pi media/raspberry
# setup ssh with the correct key on your Linux machine
mkdir .ssh
openssl rsa -in media/raspberryid_rsa -outform pem > .ssh/pi.pem
chmod 600 .ssh/pi.pem
touch .ssh/config
chmod 600 
```

Using a text editor ensure the ``~/.ssh/config`` file has these contents:

```console
Host pi
    HostName 1.2.3.4
    User pi
    IdentityFile ~/.ssh/computer.pem
```

Now that we have ssh configure you should be able to connect to your Pi using a secure key:

```console
ssh pi
```

Finally edit your Pi ``sshd_config`` to remove password authentication:

```console
sudo nano /etc/ssh/sshd_config.
# edit these values
PasswordAuthentication no
PubkeyAuthentication yes
# save and reboot your Pi
sudo reboot
```

If you reboot your Linux computer you can reconnect to the Pi file system using:

```console
ssh pi:/home/pi ~/media/raspberry
```
