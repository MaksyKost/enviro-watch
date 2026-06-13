import { MapContainer, Marker, Popup, TileLayer } from "react-leaflet";
import L from "leaflet";
import "leaflet/dist/leaflet.css";
import { DEFAULT_LAT, DEFAULT_LON, DEFAULT_REGION } from "../types";

const markerIcon = L.divIcon({
  className: "",
  html: `<div style="position:relative;display:flex;flex-direction:column;align-items:center">
    <div style="position:absolute;width:48px;height:48px;background:rgba(96,165,250,0.2);border-radius:50%;animation:ping 1.5s cubic-bezier(0,0,0.2,1) infinite"></div>
    <div style="width:16px;height:16px;background:#60a5fa;border:2px solid #1d2025;border-radius:50%;box-shadow:0 0 10px rgba(96,165,250,0.8);z-index:1"></div>
  </div>`,
  iconSize: [48, 48],
  iconAnchor: [24, 24],
});

interface RegionMapProps {
  lat?: number;
  lon?: number;
  region?: string;
  markerLabel?: string;
  metrics?: { label: string; value: string }[];
  overlayTitle?: boolean;
}

export function RegionMap({
  lat = DEFAULT_LAT,
  lon = DEFAULT_LON,
  region = DEFAULT_REGION,
  markerLabel = "WRO-01",
  metrics = [],
  overlayTitle = false,
}: RegionMapProps) {
  return (
    <div className="h-full relative overflow-hidden">
      {overlayTitle && (
        <div className="absolute top-0 left-0 w-full px-md py-sm z-[500] bg-gradient-to-b from-surface-container to-transparent pointer-events-none">
          <h2 className="font-headline-sm text-headline-sm text-on-surface drop-shadow-md">Region Map</h2>
        </div>
      )}
      <MapContainer
        center={[lat, lon]}
        zoom={11}
        scrollWheelZoom={false}
        className="h-full w-full z-0"
        style={{ background: "#32353b" }}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OSM</a>'
          url="https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png"
        />
        <Marker position={[lat, lon]} icon={markerIcon}>
          <Popup>
            <strong>{markerLabel}</strong>
            <div className="text-xs text-on-surface-variant">{region}</div>
            {metrics.map((m) => (
              <div key={m.label} className="text-xs">
                {m.label}: {m.value}
              </div>
            ))}
          </Popup>
        </Marker>
      </MapContainer>
      <div className="absolute top-1/2 left-1/2 -translate-x-1/2 translate-y-2 flex flex-col items-center pointer-events-none z-[400]">
        <div className="mt-1 bg-surface-container/90 backdrop-blur-sm border border-outline-variant px-2 py-0.5 rounded-sm font-label-sm text-label-sm text-primary whitespace-nowrap">
          {markerLabel}
        </div>
      </div>
    </div>
  );
}
