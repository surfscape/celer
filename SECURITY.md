# Security Policy

This policy outlines SurfScape's commitment to the security and privacy of Celer.

## Supported Versions

Celer is still in beta development phase which means that only the latest development branch version is supported. However the previous beta release is supported to receive security patches (but will not benefit from minor bug fixes).

| Version        | Type of Support     |
| ---------------| ---------------|
| 1.0.0 Beta 3 (In development)  | :white_check_mark: Supported |
| 1.0.0 Beta 2   | :white_check_mark: Security Patching only	  |
| 1.0.0 Beta 1   | :x: Deprecated  |

## Permissions and control

Because of Celer's nature, it requires admin privileges to run.

These privileges allow Celer almost full control over Windows, this allow's many of Celer's features to work, these include:
- Being able to clear Windows temporary files, dumps, and image/thumbnail cache.
- Read hardware information of the CPU, GPU, memory, drives, and more in real time.
- Run PowerShell commands to check for installed programs, services, handle power management, check for active Windows Defender features, and run repair tools like `DISM`, `chkdsk`, and `sfc`.
- Ability to kill and restart system and user applications.
- Run arbitrary PowerShell commands via cleaning signatures through the `Command` action type.
- Generating a dxDiag report.

## Network services and signatures

Celer makes use of what is known as [SurfScape Gateway](https://github.com/surfscape/gateway), SurfScape's official web services, which provide signatures (remote data files) through the internet to easily update various components of Celer (currently handling cleaning signatures).

The user can opt in on automatic updates of these signatures and every time Celer launches these will be downloaded and saved locally, however disabling the entirety of SurfScape Gateway is only possible starting with Celer Beta 3.

> [!IMPORTANT]
> There's currently no hash/validation of the integrity or unattended changes done to the signatures which are served through HTTPS/TLS, this in turn may allow the attacker to run arbitrary commands through the `Command` action type. This will be fixed in the future by introducing white lists of acceptable commands and having a more secure way to retrieve signatures.

Starting with Beta 3, Celer ships by default an embedded cleaning signature that is trusted. If you think a MITM attack may be happening you can force the use of the embedded signatures by going into Menu > Preferences > Cleaning options, and enable `Prefer internal signatures`. This will automatically reload the signatures in memory to the embedded ones. Do note these signatures only update with every new release of Celer.

### Pinging

Celer checks for internet connectivity during launch, this is done through pinging [Quad9](https://quad9.net/) DNS (9.9.9.9). This is also present in the Network module, where the internet connection test does a ping to [Google Public DNS](https://developers.google.com/speed/public-dns/) (8.8.8.8).

The DNS Manager may also ping once when entering the Network Module and when the user clicks the refresh latency button.

## Reporting a vulnerability

Reporting vulnerabilities is done through GitHub Security Advisory. If you found a vulnerability please [create a draft](https://github.com/surfscape/celer/security/advisories/new).

It may take between 72 hours and one week for a response.

You may also contact us through the following email `internal_surfscape@riseup.net` for other enquiries (such as not being able to report a vulnerability through GitHub, or having questions/doubts about our measures and want to do so privately).

### Scope

Celer is an advanced toolbox for Windows and needs high privileges to do its job, as mentioned
previously. This means some behaviour that would be otherwise be suspicious is intentional.
The lists below should help you decide whether something is worth reporting.

**In scope:**

- Any way for a non-administrator user, or another application, to influence what Celer deletes,
  runs, or changes.
- Tampering with the cleaning signatures, or with the way Celer fetches, stores, and parses them.
- Escaping the intended set of cleaning paths (for example, getting Celer to delete something
  outside the folders a signature declares).
- Binary or DLL planting in Celer's install directory, or hijacking the scheduled task Celer
  registers to run at startup.
- Exposure of hardware identifiers, serial numbers, or user paths to somewhere they should not go.
- Anything that causes Celer to run code it was not meant to run.

**Out of scope:**

- The fact that Celer requires administrator privileges. This is by design and documented above.
- Anything that already assumes an attacker with administrator access to the machine.
- Vulnerabilities in Windows itself, or in the tools Celer invokes (`DISM`, `SFC`, `chkdsk`,
  `cleanmgr`, `powershell`).
- Dependency advisories without a working path to exploit them through Celer.
- Social engineering, phishing, or physical access to an unlocked machine.
- Missing hardening flags or best practices with no demonstrated impact.
- The lack of signature integrity validation described above. This is already known and tracked.

### Safe harbour

We welcome responsible research on Celer and will not pursue or support legal action
against anyone who reports an issue responsibly.

To stay within this, please:

- Only test against machines you own or have permission to test on.
- Do not access, modify, or exfiltrate data belonging to other people.
- Do not degrade or disrupt SurfScape Gateway or any other service Celer depends on.
- Give us a reasonable window to fix the issue before making it public. We ask for 90 days, but if
  a fix is taking longer than expected we would rather talk about it than have you wait
  indefinitely.

If you are unsure whether something crosses a line, ask first through the email above.

> [!NOTE]
> This only covers what is ours. We cannot grant permission to test GitHub, or any other third
> party service that Celer or SurfScape happens to use.