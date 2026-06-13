import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useCallback, useEffect, useRef, useState } from "react";
import type { DataUpdate } from "../types";

const HUB_URL = `${import.meta.env.VITE_API_URL ?? ""}/hubs/dashboard`;

export interface SignalRLogEntry {
  id: string;
  time: string;
  level: "INFO" | "WARN" | "DEBUG";
  message: string;
}

export function useSignalR(onUpdate?: (update: DataUpdate) => void) {
  const [connected, setConnected] = useState(false);
  const [logs, setLogs] = useState<SignalRLogEntry[]>([]);
  const onUpdateRef = useRef(onUpdate);
  onUpdateRef.current = onUpdate;

  const pushLog = useCallback((level: SignalRLogEntry["level"], message: string) => {
    const now = new Date();
    setLogs((prev) =>
      [
        {
          id: crypto.randomUUID(),
          time: now.toLocaleTimeString("en-GB", {
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit",
            fractionalSecondDigits: 3,
          }),
          level,
          message,
        },
        ...prev,
      ].slice(0, 50),
    );
  }, []);

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl(HUB_URL)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on("DataUpdate", (update: DataUpdate) => {
      pushLog(
        "INFO",
        `Payload from ${update.region}. Temp: ${update.data.temperature}°C, Humidity: ${update.data.humidity}%, Wind: ${update.data.wind} km/h.`,
      );
      onUpdateRef.current?.(update);
    });

    connection.onreconnecting(() => {
      setConnected(false);
      pushLog("WARN", "SignalR reconnecting…");
    });

    connection.onreconnected(() => {
      setConnected(true);
      pushLog("INFO", "SignalR reconnected.");
    });

    connection.onclose(() => {
      setConnected(false);
      pushLog("WARN", "SignalR disconnected.");
    });

    connection
      .start()
      .then(() => {
        setConnected(true);
        pushLog("INFO", "Connected to dashboard hub.");
      })
      .catch((err: Error) => {
        pushLog("WARN", `Connection failed: ${err.message}`);
      });

    return () => {
      void connection.stop();
    };
  }, [pushLog]);

  return { connected, logs };
}
