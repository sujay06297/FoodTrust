export type RestaurantListItem = {
  id: number;
  name: string;
  branchName: string | null;
  address: string;
  phoneNumber: string | null;
  city: string | null;
  district: string | null;
  priceMin: number | null;
  priceMax: number | null;
  cuisineType: string | null;
  rawAverageScore: number | null;
  platformScore: number | null;
  favoriteCount: number;
  reviewCount: number;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type RestaurantSearchResult = {
  items: RestaurantListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type RestaurantRankingItem = {
  id: number;
  name: string;
  address: string;
  phoneNumber: string | null;
  rawAverageScore: number;
  platformScore: number;
  rankingScore: number;
  favoriteCount: number;
  reviewCount: number;
};

export type RestaurantDetail = {
  id: number;
  name: string;
  branchName: string | null;
  address: string;
  phoneNumber: string | null;
  city: string | null;
  district: string | null;
  latitude: number | null;
  longitude: number | null;
  openingHours: string | null;
  priceMin: number | null;
  priceMax: number | null;
  cuisineType: string | null;
  tags: string | null;
  description: string | null;
  officialUrl: string | null;
  googleMapUrl: string | null;
  status: string;
  createdAt: string;
  updatedAt: string;
};

export type RestaurantReview = {
  id: number;
  restaurantId: number;
  tasteScore: number;
  serviceScore: number;
  environmentScore: number;
  valueScore: number;
  revisitScore: number;
  averageScore: number;
  content: string;
  reviewerName: string | null;
  visitDate: string | null;
  pricePerPerson: number | null;
  diningType: string | null;
  companionType: string | null;
  status: string;
  createdAt: string;
};

export type FavoriteRestaurantSearchResult = {
  items: Array<RestaurantListItem & { restaurantId?: number; favoritedAt?: string }>;
  page: number;
  pageSize: number;
  totalCount: number;
};

export type UserSummary = {
  id: number;
  email: string;
  displayName: string;
  role: string;
  status: string;
  createdAt: string;
};

export type UserAuthResult = {
  accessToken: string;
  expiresAt: string;
  user: UserSummary;
};

export type CandidateRestaurant = {
  id: number;
  sourceSystem: string;
  sourceKey: string;
  rawName: string;
  rawAddress: string;
  rawPhoneNumber: string | null;
  suggestedName: string | null;
  status: string;
  linkedRestaurantId: number | null;
  createdAt: string;
  updatedAt: string;
};

export type CandidateRestaurantSearchResult = {
  items: CandidateRestaurant[];
  page: number;
  pageSize: number;
  totalCount: number;
};
