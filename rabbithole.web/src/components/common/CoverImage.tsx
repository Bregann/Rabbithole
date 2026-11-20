import { Center, Image } from '@mantine/core'
import { IconMusic } from '@tabler/icons-react'

interface CoverImageProps {
  src: string | null
  size: number
  radius?: 'sm' | 'md' | 'xl'
  round?: boolean
}

export default function CoverImage({ src, size, radius = 'sm', round = false }: CoverImageProps) {
  if (src === null) {
    return (
      <Center w={size} h={size} bg="dark.5" style={{ borderRadius: round ? '50%' : 4, flexShrink: 0 }}>
        <IconMusic size={size / 2.5} color="var(--mantine-color-dimmed)" />
      </Center>
    )
  }

  return <Image src={src} w={size} h={size} radius={round ? size : radius} fit="cover" style={{ flexShrink: 0 }} alt="" />
}
