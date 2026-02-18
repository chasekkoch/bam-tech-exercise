export interface HealthCheckResponse {
  status: string;
  timestamp: string;
  totalDuration: number;
  components: HealthComponent[];
}

export interface HealthComponent {
  name: string;
  status: string;
  description: string;
  duration: number;
  tags: string[];
  exception: HealthException | null;
  data: Record<string, any> | null;
}

export interface HealthException {
  message: string;
  type: string;
}

export interface ExceptionResponse {
  error: string;
  message: string;
  exceptionId: string;
  timestamp: string;
  traceId: string;
}

export interface ExceptionLogEntry {
  exceptionId: string;
  requestId: string;
  method: string;
  path: string;
  queryString: string;
  statusCode: number;
  exceptionType: string;
  message: string;
  stackTrace: string | null;
  timestampUtc: string;
  userAgent: string | null;
  remoteIp: string | null;
}

export interface TrendingException {
  exceptionType: string;
  count: number;
  messages: Array<{
    message: string;
    count: number;
  }>;
  latestException: ExceptionLogEntry | null;
}

export interface TrendingExceptionsResponse {
  enabled: boolean;
  hours: number;
  timestamp: string;
  data: TrendingException[];
}

export interface ExceptionsListResponse {
  enabled: boolean;
  data: ExceptionLogEntry[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface RequestLogEntry {
  requestId: string;
  method: string;
  path: string;
  queryString: string;
  statusCode: number;
  durationMs: number;
  timestampUtc: string;
  userAgent: string | null;
  remoteIp: string | null;
}

export interface EndpointStat {
  path: string;
  totalRequests: number;
  methods: Array<{
    method: string;
    count: number;
  }>;
  statusCodes: Array<{
    statusCode: number;
    count: number;
  }>;
  avgDurationMs: number | null;
  maxDurationMs: number | null;
}

export interface RequestStatsResponse {
  enabled: boolean;
  hours: number;
  timestamp: string;
  data: EndpointStat[];
}

export interface RequestsListResponse {
  enabled: boolean;
  data: RequestLogEntry[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export type HealthStatus = 'Healthy' | 'Degraded' | 'Unhealthy';
