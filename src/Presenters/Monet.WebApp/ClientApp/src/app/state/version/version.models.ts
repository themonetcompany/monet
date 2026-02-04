export interface VersionResponse {
  version: string;
}

export interface VersionState {
  value: string | null;
  loading: boolean;
  error: string | null;
}
