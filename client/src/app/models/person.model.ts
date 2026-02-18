export interface Person {
  personId: number;
  name: string;
  currentRank: string;
  currentDutyTitle: string;
  careerStartDate: string | null;
  careerEndDate: string | null;
}

export interface AstronautDetail {
  id: number;
  personId: number;
  currentRank: string;
  currentDutyTitle: string;
  careerStartDate: string;
  careerEndDate?: string;
}

export interface AstronautDuty {
  id: number;
  personId: number;
  rank: string;
  dutyTitle: string;
  dutyStartDate: string;
  dutyEndDate?: string;
  person?: Person;
}

export interface CreatePersonRequest {
  name: string;
}

export interface CreateAstronautDutyRequest {
  name: string;
  rank: string;
  dutyTitle: string;
  dutyStartDate: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  responseCode: number;
}
