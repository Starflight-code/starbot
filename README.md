## StarBot
StarBot is a Discord bot that does one thing well. It quickly and efficiently scrapes Reddit and XKCD.com for webcomics, memes, cat photos, and even cute anime girls.

#### How do I host StarBot?

Get the Rust toolchain installed, I'll be using cargo as a build tool. For information on how to set this up, see [the book](https://doc.rust-lang.org/stable/book/ch01-01-installation.html).

```
cargo build -r
./starbot <your-bot-token-here>
```

It's really that simple to launch it.

#### How do I get StarBot set up in my server?

It couldn't be easier, StarBot provides a bunch of slash commands to get you started. First through, you'll have to initialize it.

Type this in some Discord channel, that StarBot can see and respond in.
```
!setup
```

Now you can get your automations set up. Let's do that now!

```
/create_automation cats Reddit True
```

This creates an automation, scraping the r/cats subreddit. It's using our Reddit handler and we're only looking for posts with cat photos in them.

Now, we have to schedule it in the channel we're current in.

```
/add_schedule cats * * * 0/8 0 Cat Automation
```

This runs our cats automation 3 times per day, with a display name of Cat Automation. The title of our embeds will be "Cat Automation #{iterator-here}".

You're finished and StarBot is set up.

Feel free to open an issue if you find something that doesn't work correctly or open pull a request if you'd like to contribute.
