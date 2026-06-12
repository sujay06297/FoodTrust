import type { Metadata } from "next";
import { CandidateRestaurantsAdmin } from "./candidate-restaurants-admin";

export const metadata: Metadata = {
  title: "候選餐廳管理",
  description: "審核外部匯入的候選餐廳資料",
};

export default function CandidateRestaurantsPage() {
  return <CandidateRestaurantsAdmin />;
}
