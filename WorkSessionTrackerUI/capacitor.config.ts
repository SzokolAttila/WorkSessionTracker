import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.example.app',
  appName: '\x17WorkSessionTracker',
  webDir: 'dist',
  server: {
    url: 'http://192.168.1.38:5173', 
    cleartext: true                  
  }
};

export default config;
