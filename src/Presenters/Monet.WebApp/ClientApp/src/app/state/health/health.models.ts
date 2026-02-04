export interface HealthStatus {
  status: string;
  timestamp: string;
}

export interface HealthState {
  data: HealthStatus | null;
  loading: boolean;
  error: string | null;
}
