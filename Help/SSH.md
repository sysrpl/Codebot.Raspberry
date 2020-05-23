# Secure Shell Host (ssh)

When working with your Raspberry Pi an almost indispensible tool is the ssh utility. T

The instructions below can guide you in setting up `ssh` using secure key files and mounting Pi folders on your desktop.

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
# make our new key the one and only and set permissions
mv id_rsa.pub authorized_keys
chmod 600 *
```
</details>
<details>
  <summary>How to configure your Linux computer to use the Pi key files</summary>
  
On your Linux machine temporarily connect to your Pi using ssh with password authentication and enabled sshfs for secure file transfers. Where you see `1.2.3.4` substitute it with  the ip address of your Pi:

```bash
# go to you home folder and install the tool for sshfs
sudo apt install sshfs
# create a mount point for your Raspberry Pi home folder
mkdir -p ~/media/raspberry
sshfs pi@1.2.3.4:/home/pi ~/media/raspberry
# setup ssh with the correct key on your Linux machine
mkdir ~/.ssh
cd ~/.ssh
cp ~/media/raspberry/.ssh/id_rsa id_rsa_pi
openssl rsa -in id_rsa_pi -outform pem  > pi.pem
# delete the temporary file and set permissions
rm id_rsa_pi
touch .ssh/config
chmod 600 * 
```

On your Linux machine use a text editor to add these lines to  `~/.ssh/config`:

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

Finally on your Pi edit `sshd_config` to remove password authentication:

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

### See also

[Table of Contents](TOC.md)