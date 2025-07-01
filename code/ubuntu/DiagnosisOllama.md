➜  ~ systemctl status ollama
● ollama.service - Ollama Service
     Loaded: loaded (/etc/systemd/system/ollama.service; enabled; preset: enabled)
     Active: active (running) since Mon 2025-06-30 10:19:26 CST; 4h 43min ago
 Invocation: 01012dc93a844a0dbd5fa888d31b4d01
   Main PID: 2122 (ollama)
      Tasks: 26 (limit: 75254)
     Memory: 3.9G (peak: 3.9G)
        CPU: 1min 19.176s
     CGroup: /system.slice/ollama.service
             └─2122 /usr/local/bin/ollama serve

jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |   23.088373ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |    7.166358ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |    7.304807ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |    9.041371ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |     21.4163ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |   27.969391ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |   28.347473ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |   19.713002ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |   20.648255ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |   21.452712ms |       127.0.0.1 | POST >
➜  ~ wich ollama
zsh: command not found: wich
➜  ~ which ollama
/usr/local/bin/ollama
➜  ~ sudo -u claude ollama list
[sudo] password for abel: 
NAME                       ID              SIZE      MODIFIED       
qwen2.5:3b                 357c53fb659c    1.9 GB    37 minutes ago    
llama3.2:3b                a80c4f17acd5    2.0 GB    41 minutes ago    
nomic-embed-text:latest    0a109f422b47    274 MB    11 months ago     
starcoder2:3b              f67ae0f64584    1.7 GB    11 months ago     
llama3:latest              365c0bd3c000    4.7 GB    11 months ago     
➜  ~ Journalctl -u ollama -n 20
zsh: command not found: Journalctl
➜  ~ journalctl -u ollama -n 20
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |      418.92µs |       127.0.0.1 | GET  >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |   16.418632ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |    5.480247ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |     643.397µs |       127.0.0.1 | GET  >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |    5.187938ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |    9.920066ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |   10.717474ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |    6.132654ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |   23.088373ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |    7.166358ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |    7.304807ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |    9.041371ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |     21.4163ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |   27.969391ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |   28.347473ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |   19.713002ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |   20.648255ms |       127.0.0.1 | POST >
jun 30 14:53:17 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 14:53:17 | 200 |   21.452712ms |       127.0.0.1 | POST >
jun 30 15:03:59 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 15:03:59 | 200 |      15.716µs |       127.0.0.1 | HEAD >
jun 30 15:03:59 abel-System-Product-Name ollama[2122]: [GIN] 2025/06/30 - 15:03:59 | 200 |     444.174µs |       127.0.0.1 | GET  >
lines 1-20/20 (END)

