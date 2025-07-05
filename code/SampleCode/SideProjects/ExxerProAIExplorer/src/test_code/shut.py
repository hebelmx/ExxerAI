import time
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler
import os
import platform

class Watcher:
    def __init__(self, directory_to_watch, shutdown_time=300):
        self.observer = Observer()
        self.shutdown_time = shutdown_time
        self.directory_to_watch = directory_to_watch
        self.last_activity = time.time()
        self.shutdown_scheduled = False

    def run(self):
        event_handler = Handler(self)
        self.observer.schedule(event_handler, self.directory_to_watch, recursive=True)
        self.observer.start()
        try:
            while True:
                time.sleep(5)
                current_time = time.time()
                if (current_time - self.last_activity) > self.shutdown_time:
                    if not self.shutdown_scheduled:
                        print("No activity detected, scheduling shutdown in 10 minutes.")
                        self.schedule_shutdown()
                        self.shutdown_scheduled = True
                else:
                    if self.shutdown_scheduled:
                        print("Activity detected, canceling scheduled shutdown.")
                        self.cancel_shutdown()
                        self.shutdown_scheduled = False
        except KeyboardInterrupt:
            self.observer.stop()
            print("Observer Stopped")

        self.observer.join()

    def schedule_shutdown(self):
        if platform.system() == 'Windows':
            os.system("shutdown /s /t 600")
        elif platform.system() == 'Linux':
            os.system("sudo shutdown -h +10")

    def cancel_shutdown(self):
        if platform.system() == 'Windows':
            os.system("shutdown /a")
        elif platform.system() == 'Linux':
            os.system("sudo shutdown -c")

class Handler(FileSystemEventHandler):
    def __init__(self, watcher):
        self.watcher = watcher

    def on_any_event(self, event):
        self.watcher.last_activity = time.time()
        if self.watcher.shutdown_scheduled:
            print("Activity detected, resetting shutdown timer.")
            self.watcher.cancel_shutdown()
            self.watcher.shutdown_scheduled = False

if __name__ == '__main__':
    # Customize the path and the idle time before initiating the shutdown delay
    w = Watcher(directory_to_watch='/home/abel/projects/ExxerProAIExplorer', shutdown_time=300)  # 300 seconds = 5 minutes
    w.run()
