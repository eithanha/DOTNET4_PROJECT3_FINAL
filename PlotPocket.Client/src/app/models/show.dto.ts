export enum ShowType {
  Movie = 'Movie',
  TvShow = 'TvShow',
}

export interface ShowDto {
  id: number;
  title: string;
  overview: string;
  posterPath: string;
  rating: number;
  releaseDate?: Date;
  type: ShowType;
  isWatchlisted: boolean;
  isWatched: boolean;
}
